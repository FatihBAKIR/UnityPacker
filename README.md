# UnityPacker

UnityPacker is a small tool that can create `UnityPackage` files without a Unity installation.

Usage is very simple:

    ./UnityPacker *directory to pack* *destination pack name*
    
For example:

    ./UnityPacker . Package
    
Will produce a `Package.unitypackage` from the contents current directory recursively in the current directory.

+ When used with `meaningfulHashes` option, packer will even maintain prefab components 
and even complete scenes if they are included in the directory.

+ It will automatically skip `.meta` files, however you can add more using the `extensions` option. 
You can skip whole directories with the `directories` option.

+ Sizes of the packages exported from Unity itself and UnityPacker are usually the same, 
however for big packages, UnityPacker will produce smaller packages due to not including unnecessary meta files directly.
