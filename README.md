# XR - Experimental Runtime

Runtime Experimental that downloads/compile and executes C# codes modified named (.xr) in a decentralized way from a single file or url

### Clone
```
git clone https://github.com/d3c0d3d/XR.git
```

### Build
```
dotnet run
```

## Usage

### Example
```
run=https://pastebin.com/raw/MzYm70ch
```
or
```
run=file.xr
```
![alt text](https://github.com/d3c0d3d/XR/raw/master/images/screenshot.png?raw=true)

Experiment Code (file.xr)
```cs
// Class A
ImportFrom("https://pastebin.com/raw/4vT6zwj0");

// Class B
ImportFrom("https://pastebin.com/raw/GHEjrhci");

Print(A.PrintMsg()); 
PrintLn(B.PrintMsg());
```
