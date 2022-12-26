# SecureLookup

## Notes for parameters
* Space between key and value is not allowed.
  * ```-name Hello``` will not processed correctly as ```args.GetSwitch("name")``` will return *empty string* instead of ```"Hello"```
* '-' and '/' character is registered as parameter prefix.
  * ```-param``` and ```/param``` are considered same
* Key-value separator '```:```', '```=```' are supported, but it isn't not mandatory.
  * ```-nameHello```, ```-name:Hello```, ```-name=Hello``` does the same thing
* You can use double quote (") to specify string which contains whitespaces.
  * ```-name"Hello World!"```
    * ```args.GetSwitch("name")``` will return ```"Hello World!"```,   
    instead of ```"Hello```

### Conclusion
All of these are same
* ```-name"Hello World!"```
* ```/name"Hello World!"```
* ```-name:"Hello World!"```
* ```/name:"Hello World!"```
* ```-name="Hello World!"```
* ```/name="Hello World!"```

These are wrong
* ```-name "Hello World!"```
* ```/name "Hello World!"```