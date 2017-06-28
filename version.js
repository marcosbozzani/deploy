var shell = new ActiveXObject("Shell.Application");
var filesys = new ActiveXObject("Scripting.FileSystemObject");

var file = WScript.Arguments(0);
var path = filesys.GetParentFolderName(file) || filesys.GetAbsolutePathName(".");

var folder = shell.NameSpace(path);
var item = folder.ParseName(file);

for (var i = 0; i < 300; i++) {
    var name = folder.GetDetailsOf(file, i);
    var value = folder.GetDetailsOf(item, i);
    
    if (name.toLowerCase() == "product version")
    {
        WScript.StdOut.Write(value);
        break;
    }
}
