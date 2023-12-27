# Unichain.P2P

## Request

A request packet is used to ask for information from
another node or send important data. It also can be a broadcast,
where the sender should not expect a response and must send
the same request for all its known peers.

A request is made of primarily of many parts:

* The current protocol version that the sender is using.
* A [RequestMethod](./RequestMethod.cs). Similar to HTTP's GET, POST, etc.
* A Route. It is a path that identifies what endpoint in the node this 
request should be sent to.
* Information about the sender. For maximum compatibility, it should include
private and public IP, port and a unique node indentifier. This information
is used to enable nodes in the same computer or same networks to efficiently
communicate with each other.
* Whether this request is originating from a broadcast request or not.
* A collection of contents. Each content has a set of headers and a payload.

### Structure

The sequence structure of a request is as follows:

> **Note:** All strings in the payload have a size prefix, so you
shouldn't read string until a null terminator(\0) is found.

| Order | Description | Type | Size |
| ----- | ----------- | ---- | ---- |
| 1	| Protocol Version          | int       | 4 bytes  |
| 2	| Request Method            | int       | 4 bytes  |
| 3	| Route                     | string    | Variable |
| 4 | Ip Version(IPV4/IPV6)     | bool      | 1 byte   |
| 5 | Public Ip                 | string    | Variable |
| 6 | Private Ip(empty if IPV6) | string    | Variable |
| 7 | Is Broadcast              | bool      | 1 byte   |
| 8 | Contents Count            | int       | 4 bytes  |
| 9 | Contents                  | Content[] | Variable |

## Content 

A packet content is merely a collection of headers and a payload.
An entire content can be read/written with funcions from the
struct [Content](./Content.cs) itself.

### Structure

| Order | Description | Type | Size |
| ----- | ----------- | ---- | ---- |
| 1 | Headers Count   | int      | 4 bytes  |
| 2 | Headers         | Header[] | Variable |
| 3 | Payload Length  | int      | 4 bytes  |
| 4 | Payload         | byte[]   | Variable |

## Content Headers

The headers are included in each content to identify the content's type
and any other metadata that is needed to process the content. It can be
the file name, in case it is an attachment, the priority of a new
transaction, compression algorithm of the payload, encryption, etc.

A header is constituted of a key and a value. Both are strings. Beware
that can be a limit of key/value sizes, as it is prudent to keep them
at a minimum size to ensure fast communication.

### Structure

The strings here could omit the size prefix, as they are already
included in the content structure, but are included for simplicity.

| Order | Description | Type | Size |
| ----- | ----------- | ---- | ---- |
| 1 | Key   | string | Variable |
| 2 | Value | string | Variable |


## Content Payload

The content payload is basically the data being sent. It is an unsigned
integer indicating the size of the payload, followed by the payload itself.
Note that the payload may be compressed and/or encrypted, depending on the
headers information.

## Response

A response is simpler than a request as it has a limitation of only one
content. It also has a status code, indicating whether the request was
successful or an error occurred.

### Structure

| Order | Description | Type | Size |
| ----- | ----------- | ---- | ---- |
| 1 | Protocol Version | int | 4 bytes |
| 2 | Status Code      | int | 4 bytes |
| 3 | Content          | Content | Variable |
