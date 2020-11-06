
public class CodeButton {
    public string code;
    public string refId;
    public string desc;
    public bool enabled;

    public CodeButton(string _code, string _refId, bool _enabled){
        code = _code;
        refId = _refId;
        enabled = _enabled;
    }

    public void ChangeEnabled(bool _en){
        enabled = _en;
    }

    public string GetCode(){
        return code;
    }

    public string GetRefId(){
        return refId;
    }

    public bool IsEnabled(){
        return enabled;
    }
}
