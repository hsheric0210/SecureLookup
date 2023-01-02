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

## Database XML structure
Byte array entries are usually encoded with [Z85 encoding](https://rfc.zeromq.org/spec/32/)

### Outer database
```xml
<?xml version="1.0" encoding="utf-8"?>
<db xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <primaryPasswordHashing>
    <algorithm>(string: Primary password hashing algorithm)</algorithm>
    <salt>(Z85: Primary password hashing salt; changes every password changes)</salt>
    <properties>(serializedProps: Primary password hashing properties))</properties>
  </primaryPasswordHashing>
  <primaryPasswordHashSize>(int: Primary password hashing desired length size; changes every password changes)</primaryPasswordHashSize>
  <secondaryPasswordHashing>
    <algorithm>(string: Secondary password hashing algorithm)</algorithm>
    <salt>(Z85: Secondary password hashing salt; changes every encryption)</salt>
    <properties>(serializedProps: Secondary password hashing proprties)</properties>
  </secondaryPasswordHashing>
  <compression>
    <algorithm>(string: Compression algorithm name)</algorithm>
    <properties>(serializedProps: Compressor/decompressor properties)</properties>
  </compression>
  <hash>
    <algorithm>(string: Hashing algorithm)</algorithm>
    <hash>(hexString: Hash)</hash>
  </hash>
  <encryption>
    <algorithm>(string: Encryption algorithm name)</algorithm>
    <seed>(Z85: Encryption IV or Nonce; changes every encryption)</seed>
    <tag>(Z85: AEAD encryption tag; could be empty)</tag>
    <data>(Z85: Encrypted inner database)</data>
  </encryption>
</db>
```

### Inner database
Inner database is encrypted by default. This decrypted-form xml file can be obtained with '-ExportFile' switch
```xml
<?xml version="1.0" encoding="utf-8"?>
<root xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <entries>
    <entry>
      <name>(string: Entry name)</name>
      <originalFileName>(string: The original file or folder name)</originalFileName>
      <archiveFileName>(string: Archive file name)</archiveFileName>
      <password>(string: Archive password)</password>
      <urls>
        <url>(string: url 1)</url>
        <url>(string: url 2)</url>
        <url>(string: url 3)</url>
        <url>...</url>
      </urls>
      <notes>
        <note>(string: note 1)</note>
        <note>(string: note 2)</note>
        <note>(string: note 3)</note>
        <note>...</note>
      </notes>
    </entry>
    <entry>
        ...
    </entry>
  </entries>
  <generatedFileNames>
    <fileName>(string: generated file name 1)</fileName>
    <fileName>(string: generated file name 2)</fileName>
    <fileName>(string: generated file name 3)</fileName>
    <fileName>...</fileName>
  </generatedFileNames>
</root>
```

## Supported Algorithms

### Password Hashing
* PBKDF2
  * PBKDF2-HMAC-SHA1
  * PBKDF2-HMAC-SHA256
  * PBKDF2-HMAC-SHA512
  * PBKDF2-HMAC-SHA3-256
  * PBKDF2-HMAC-SHA3-512
* Argon2
  * Argon2i
  * Argon2d
  * Argon2id
* Bcrypt - Buggy: Doesn't limit password to 72-bytes thus using it as primary password hashing with password >72 bytes or secondary password hashing will be likely to cause errors
* Scrypt

### Compression
* Deflate
* Gzip
* LZMA - Buggy: LzmaStream always throw 'Data Error' on decompression
* PPMd

### Hashing (for integrity check)
* SHA2-512
* SHA3-512

### Encryption
* AES-CBC
* AES-GCM

## SerializedProps

### Password Hashing
Also applies to '-PrimaryPasswordHashingProperties' and '-SecondaryPasswordHashingProperties' parameter on database creation
* PBKDF2 - ```iterations=<# of iterations>```
    > In 2021, OWASP recommended to use 310,000 iterations for PBKDF2-HMAC-SHA256 and 120,000 for PBKDF2-HMAC-SHA512. [#](https://en.wikipedia.org/wiki/PBKDF2)
* Argon2 - ```iterations=<# of iterations>;memorySizeKb=<memory size in KB>;parallelism=<parallelism>```
    > The Argon2 documentation recommends 0.5 seconds for authentication. Libsodium's documentation recommends 1 second for web applications and 5 seconds for desktop applications.
    > I personally wouldn't recommend anything more than 1 second or your users will hate you and logins will end up DoS'ing your application. Anything less than 0.5 seconds is a certain security failure. [#](https://www.twelve21.io/how-to-choose-the-right-parameters-for-argon2/)
* Bcrypt - ```cost=<cost in 4..31>```
* Scrypt - ```N=<cost factor>;r=<block size factor>;p=<parallelization factor>```

### Compression
Also applies to '-DatabaseCompressionProperties' parameter on database creation
* Deflate - ```x=<compression level 0..9>```
* Gzip - No props available
* LZMA - ```d=<dictionary size>;mf=<match finder; 'bt4' by default>;fb=<# of fast bytes; 32 by default>;lc=<literal context bits>;lp=<literal pos bits>;pb=<pos state bits>```
* PPMd - ```mem=<memory size>;o=<model order>```