using UnityEngine;

public class ScratchCard : MonoBehaviour {
    public Camera MainCamera;
    public Transform Surface;
    public Quality RenderTextureQuality = Quality.Medium;
    public Material Eraser;
    public Material Progress;
    public Material ScratchSurface;

    public enum Quality {
        Low = 4,
        Medium = 2,
        High = 1
    }

    public bool IsScratching {
        get {
            return isScratching;
        }
    }

    private Camera thisCamera;
    private RenderTexture renderTexture;
    private Bounds scratchBounds;
    private Renderer scratchRenderer;
    private Rect textureRect = new Rect(0f, 0f, 1f, 1f);
    private Vector2 imageSize;
    private Vector2 eraseStartPosition;
    private Vector2 eraseEndPosition;
    private Vector2 erasePosition;
    private bool isFirstFrame = true;
    private bool isScratching;
    private bool isStartPosition = true;
    private int fingerId = -1;

    private const string MaskTexProperty = "_MaskTex";
    private const string MainTexProperty = "_MainTex";

    void Start() {
        GetScratchBounds();
        CreateRenderTexture();
    }

    public void ResetScratchCard() {
        fingerId = -1;
        textureRect = new Rect(0f, 0f, 1f, 1f);
        isStartPosition = true;
        isFirstFrame = true;
        GetScratchBounds();
        CreateRenderTexture();
    }

    void OnPostRender() {
        if (isFirstFrame) {
            GL.Clear(false, true, Color.clear);
            isFirstFrame = false;
        }

        if (isScratching) {
            if (eraseStartPosition == eraseEndPosition) {
                ScratchHole();
            } else {
                ScratchLine();
            }
        }
    }

    void Update() {
        if (Input.touchSupported) {
            foreach (Touch touch in Input.touches) {
                if (touch.phase == TouchPhase.Began && fingerId == -1) {
                    fingerId = touch.fingerId;
                    isScratching = false;
                    isStartPosition = true;
                }
                if (touch.fingerId == fingerId) {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
                        OnScratch(touch.position);
                    }
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
                        fingerId = -1;
                        isScratching = false;
                    }
                }
            }
        } else {
            if (Input.GetMouseButtonDown(0)) {
                isScratching = false;
                isStartPosition = true;
            }
            if (Input.GetMouseButtonUp(0)) {
                isScratching = false;
            }
            if (Input.GetMouseButton(0)) {
                OnScratch(Input.mousePosition);
            }
        }
    }

    private void OnScratch(Vector2 position) {
        var clickPosition = MainCamera.ScreenToWorldPoint(position);
        var surfaceLocalClickPosition = Surface.InverseTransformPoint(clickPosition);
        var clickLocalPosition = new Vector3(surfaceLocalClickPosition.x * Surface.lossyScale.x, surfaceLocalClickPosition.y * Surface.lossyScale.y);
        var boundsSize = new Vector3(scratchBounds.size.x * Surface.lossyScale.x, scratchBounds.size.y * Surface.lossyScale.y);
        var bottomLeftLocalPosition = Surface.InverseTransformPoint(Surface.localPosition - boundsSize / 2f);
        var scratchSurfaceClickLocalPosition = clickLocalPosition - bottomLeftLocalPosition;
        var PPI = new Vector2(
            imageSize.x / scratchBounds.size.x / Surface.lossyScale.x,
            imageSize.y / scratchBounds.size.y / Surface.lossyScale.y
            );

        erasePosition = new Vector2(
            scratchSurfaceClickLocalPosition.x * Surface.lossyScale.x * PPI.x,
            scratchSurfaceClickLocalPosition.y * Surface.lossyScale.y * PPI.y
            );

        if (isStartPosition) {
            eraseEndPosition = eraseStartPosition;
            eraseStartPosition = erasePosition;
        } else {
            eraseEndPosition = erasePosition;
        }
        isStartPosition = !isStartPosition;

        if (!isScratching) {
            eraseEndPosition = eraseStartPosition;
            isScratching = true;
        }
    }

    private void CreateRenderTexture() {
        thisCamera = GetComponent<Camera>();
        if (thisCamera != null) {
            var renderTextureSize = new Vector2(imageSize.x / (float)RenderTextureQuality, imageSize.y / (float)RenderTextureQuality);
            renderTexture = new RenderTexture((int)renderTextureSize.x, (int)renderTextureSize.y, 0, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            ScratchSurface.SetTexture(MaskTexProperty, renderTexture);
            Progress.SetTexture(MainTexProperty, renderTexture);
            thisCamera.targetTexture = renderTexture;
        } else {
            Debug.LogError("Camera not found!");
        }
    }

    private void GetScratchBounds() {
        scratchRenderer = Surface.GetComponent<Renderer>();
        if (scratchRenderer != null) {
            imageSize = new Vector2(scratchRenderer.sharedMaterial.mainTexture.width, scratchRenderer.sharedMaterial.mainTexture.height);
            scratchBounds = scratchRenderer.bounds;
        } else {
            Debug.LogError("Can't find Renderer Component!");
        }
    }

    private void DrawQuad(Rect positionRect) {
        GL.TexCoord2(textureRect.xMin, textureRect.yMax);
        GL.Vertex3(positionRect.xMin, positionRect.yMax, 0f);
        GL.TexCoord2(textureRect.xMax, textureRect.yMax);
        GL.Vertex3(positionRect.xMax, positionRect.yMax, 0f);
        GL.TexCoord2(textureRect.xMax, textureRect.yMin);
        GL.Vertex3(positionRect.xMax, positionRect.yMin, 0f);
        GL.TexCoord2(textureRect.xMin, textureRect.yMin);
        GL.Vertex3(positionRect.xMin, positionRect.yMin, 0f);
    }

    private void ScratchHole() {
        var positionRect = new Rect(
            (erasePosition.x - 0.5f * Eraser.mainTexture.width) / imageSize.x,
            (erasePosition.y - 0.5f * Eraser.mainTexture.height) / imageSize.y,
            Eraser.mainTexture.width / imageSize.x,
            Eraser.mainTexture.height / imageSize.y
        );

        GL.PushMatrix();
        GL.LoadOrtho();
        Eraser.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(Color.white);
        DrawQuad(positionRect);
        GL.End();
        GL.PopMatrix();
    }

    private void ScratchLine() {
        GL.PushMatrix();
        GL.LoadOrtho();
        Eraser.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.Color(Color.white);

        var holesCount = (int)Vector2.Distance(eraseStartPosition, eraseEndPosition) / (int)RenderTextureQuality;
        for (int i = 0; i < holesCount; i++) {
            var holePosition = eraseStartPosition + (eraseEndPosition - eraseStartPosition) / holesCount * i;
            var positionRect = new Rect(
                (holePosition.x - 0.5f * Eraser.mainTexture.width) / imageSize.x,
                (holePosition.y - 0.5f * Eraser.mainTexture.height) / imageSize.y,
                Eraser.mainTexture.width / imageSize.x,
                Eraser.mainTexture.height / imageSize.y
            );
            DrawQuad(positionRect);
        }
        GL.End();
        GL.PopMatrix();
    }
}