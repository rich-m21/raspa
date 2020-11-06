using System.Collections.Generic;
using System;
[Serializable]
public class LoginResponseData{
    public string auth;
    public string status;
    public List<CodesData> codes;
    public bool notify;
    public string lastWonPrize;
}
