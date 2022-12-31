# SecureLookup

## Random string generation dictionary

Predefined dictionary names:
* LowerAlpha - ```abcdefghijklmnopqrstuvwxyz```
* UpperAlpha - ```ABCDEFGHIJKLMNOPQRSTUVWXYZ```
* Numeric - ```0123456789```
* Special - ```!#$%&'()*+,-./:;<=>?@[]^_`{}~```
* LowerAlphaNumeric - ```LowerAlpha + Numeric```
* UpperAlphaNumeric - ```UpperAlpha + Numeric```
* MixedAlphaNumeric (AlphaNumeric) - ```LowerAlpha + UpperAlpha + Numeric + Special```

Unique dictionary support is available. It can be enabled by providing the full character set.
>Example: Using following parameter
>```-dict=abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789```
>Will generate string such as
>* ```bKosRv0lsZ55k7dtfF3h0ydthcc0JH3P```
>* ```poCJ8GiJV3ZWTqpwZ7xC9GEcwcV2oXe5```
> ...

>Example: Using following parameter
>```-dict=Il1```
>Will generate string such as
>* IllI1II1lII1ll11ll111IlI
>* 1I1IlllllI1l11l1II1Il1II
>* IIll1Il11IIl111I1lllI1lI
> ...

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