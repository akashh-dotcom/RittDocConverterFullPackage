<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Protected.aspx.cs" Inherits="SPExample.Protected" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h1>Hello World!</h1>
    </div>
    
    <div>
        <h1>Session Keys</h1>
        <asp:Label runat="server" ID="SessionKeys"></asp:Label><br/><br/><br/><br/>
        
        <asp:Label runat="server" ID="SessionValues"></asp:Label>
    </div>
    
    <div>
        <h1>Athens Parameters</h1><br/>
        Organization Id: <asp:Label runat="server" ID="AthensIdentifier"></asp:Label><br/>
              User Name: <asp:Label runat="server" ID="AthensUsername"></asp:Label><br/>
         Persistent UID: <asp:Label runat="server" ID="AthensPersistentUID"></asp:Label>
    </div>
    
    <div>
        <h1>Encrypted</h1><br/>
             Time Stamp: <asp:Label runat="server" ID="LabelTimeStamp"></asp:Label><br/>
        Organization Id: <asp:Label runat="server" ID="EncryptAthensIdentifier"></asp:Label><br/>
              User Name: <asp:Label runat="server" ID="EncryptAthensUsername"></asp:Label><br/>
         Persistent UID: <asp:Label runat="server" ID="EncryptAthensPersistentUID"></asp:Label>
    </div>
    </form>
</body>
</html>
