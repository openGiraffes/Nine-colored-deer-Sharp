var JVM = require("node-jvm");
var jvm = new JVM();
jvm.setLogLevel(7);
var ret = jvm.loadJarFile('./minijvm_rt.jar')
console.log(ret)
var entryPointClassName = jvm.loadJarFile("./Anyview4.0.jar");
console.log(entryPointClassName)
jvm.setEntryPointClassName(entryPointClassName);
jvm.on("exit", function(code) {
    process.exit(code);
});
jvm.run([0]);
