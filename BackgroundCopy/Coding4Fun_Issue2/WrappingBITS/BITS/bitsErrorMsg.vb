Option Strict On
Option Explicit On 

Namespace BITS

    Public Class Errors

        '*
        '* MessageId: BG_E_NOT_FOUND
        '*
        '* MessageText:
        '*
        '*  The requested item was not found.
        '*
        Const BG_E_NOT_FOUND As Int64 = &H80200001

        '*
        '* MessageId: BG_E_INVALID_STATE
        '*
        '* MessageText:
        '*
        '*  The requested action is not allowed in the current state.
        '*
        Const BG_E_INVALID_STATE As Int64 = &H80200002

        '*
        '* MessageId: BG_E_EMPTY
        '*
        '* MessageText:
        '*
        '*  The item is empty.
        '*
        Const BG_E_EMPTY As Int64 = &H80200003

        '*
        '* MessageId: BG_E_FILE_NOT_AVAILABLE
        '*
        '* MessageText:
        '*
        '*  The file is not available.
        '*
        Const BG_E_FILE_NOT_AVAILABLE As Int64 = &H80200004

        '*
        '* MessageId: BG_E_PROTOCOL_NOT_AVAILABLE
        '*
        '* MessageText:
        '*
        '*  The protocol is not available.
        '*
        Const BG_E_PROTOCOL_NOT_AVAILABLE As Int64 = &H80200005

        '*
        '* MessageId: BG_S_ERROR_CONTEXT_NONE
        '*
        '* MessageText:
        '*
        '*  An error has not occured.
        '*
        Const BG_S_ERROR_CONTEXT_NONE As Int64 = &H200006

        '*
        '* MessageId: BG_E_ERROR_CONTEXT_UNKNOWN
        '*
        '* MessageText:
        '*
        '*  The error occured in an unknown location.
        '*
        Const BG_E_ERROR_CONTEXT_UNKNOWN As Int64 = &H80200007

        '*
        '* MessageId: BG_E_ERROR_CONTEXT_GENERAL_QUEUE_MANAGER
        '*
        '* MessageText:
        '*
        '*  The error occured in the queue manager.
        '*
        Const BG_E_ERROR_CONTEXT_GENERAL_QUEUE_MANAGER As Int64 = &H80200008

        '*
        '* MessageId: BG_E_ERROR_CONTEXT_LOCAL_FILE
        '*
        '* MessageText:
        '*
        '*  The error occured while processing the local file.
        '*
        Const BG_E_ERROR_CONTEXT_LOCAL_FILE As Int64 = &H80200009

        '*
        '* MessageId: BG_E_ERROR_CONTEXT_REMOTE_FILE
        '*
        '* MessageText:
        '*
        '*  The error occured while processing the remote file.
        '*
        Const BG_E_ERROR_CONTEXT_REMOTE_FILE As Int64 = &H8020000A

        '*
        '* MessageId: BG_E_ERROR_CONTEXT_GENERAL_TRANSPORT
        '*
        '* MessageText:
        '*
        '*  The error occured in the transport layer.
        '*
        Const BG_E_ERROR_CONTEXT_GENERAL_TRANSPORT As Int64 = &H8020000B

        '*
        '* MessageId: BG_E_ERROR_CONTEXT_QUEUE_MANAGER_NOTIFICATION
        '*
        '* MessageText:
        '*
        '*  The error occured while processing the notification callback.
        '*
        Const BG_E_ERROR_CONTEXT_QUEUE_MANAGER_NOTIFICATION As Int64 = &H8020000C

        '*
        '* MessageId: BG_E_DESTINATION_LOCKED
        '*
        '* MessageText:
        '*
        '*  The destination volume is locked.
        '*
        Const BG_E_DESTINATION_LOCKED As Int64 = &H8020000D

        '*
        '* MessageId: BG_E_VOLUME_CHANGED
        '*
        '* MessageText:
        '*
        '*  The destination volume changed.
        '*
        Const BG_E_VOLUME_CHANGED As Int64 = &H8020000E

        '*
        '* MessageId: BG_E_ERROR_INFORMATION_UNAVAILABLE
        '*
        '* MessageText:
        '*
        '*  Error information is unavailable.
        '*
        Const BG_E_ERROR_INFORMATION_UNAVAILABLE As Int64 = &H8020000F

        '*
        '* MessageId: BG_E_NETWORK_DISCONNECTED
        '*
        '* MessageText:
        '*
        '*  No network connection is active at this time.
        '*
        Const BG_E_NETWORK_DISCONNECTED As Int64 = &H80200010

        '*
        '* MessageId: BG_E_MISSING_FILE_SIZE
        '*
        '* MessageText:
        '*
        '*  The server did not return the file size. The URL may point to dynamic content.
        '*
        Const BG_E_MISSING_FILE_SIZE As Int64 = &H80200011

        '*
        '* MessageId: BG_E_INSUFFICIENT_HTTP_SUPPORT
        '*
        '* MessageText:
        '*
        '*  The server does not support HTTP 1.1.
        '*
        Const BG_E_INSUFFICIENT_HTTP_SUPPORT As Int64 = &H80200012

        '*
        '* MessageId: BG_E_INSUFFICIENT_RANGE_SUPPORT
        '*
        '* MessageText:
        '*
        '*  The server does not support the Range header.
        '*
        Const BG_E_INSUFFICIENT_RANGE_SUPPORT As Int64 = &H80200013

        '*
        '* MessageId: BG_E_REMOTE_NOT_SUPPORTED
        '*
        '* MessageText:
        '*
        '*  Remote use of BITS is not supported.
        '*
        Const BG_E_REMOTE_NOT_SUPPORTED As Int64 = &H80200014

        '*
        '* MessageId: BG_E_NEW_OWNER_DIFF_MAPPING
        '*
        '* MessageText:
        '*
        '*  The drive mapping for the job are different for the current owner then the previous owner.
        '*
        Const BG_E_NEW_OWNER_DIFF_MAPPING As Int64 = &H80200015

        '*
        '* MessageId: BG_E_NEW_OWNER_NO_FILE_ACCESS
        '*
        '* MessageText:
        '*
        '*  The new owner has insufficient access to the temp files.
        '*
        Const BG_E_NEW_OWNER_NO_FILE_ACCESS As Int64 = &H80200016

        '*
        '* MessageId: BG_S_PARTIAL_COMPLETE
        '*
        '* MessageText:
        '*
        '*  Some files were incomplete and were deleted.
        '*
        Const BG_S_PARTIAL_COMPLETE As Int64 = &H200017

        '*
        '* MessageId: BG_E_PROXY_LIST_TOO_LARGE
        '*
        '* MessageText:
        '*
        '*  The proxy list may not be longer then 32767 characters.
        '*
        Const BG_E_PROXY_LIST_TOO_LARGE As Int64 = &H80200018

        '*
        '* MessageId: BG_E_PROXY_BYPASS_LIST_TOO_LARGE
        '*
        '* MessageText:
        '*
        '*  The proxy bypass list may not be longer then 32767 characters.
        '*
        Const BG_E_PROXY_BYPASS_LIST_TOO_LARGE As Int64 = &H80200019

        '*
        '* MessageId: BG_S_UNABLE_TO_DELETE_FILES
        '*
        '* MessageText:
        '*
        '*  Unable to delete all the temporary files.
        '*
        Const BG_S_UNABLE_TO_DELETE_FILES As Int64 = &H20001A

        '*
        '* MessageId: BG_E_HTTP_ERROR_100
        '*
        '* MessageText:
        '*
        '*  The request can be continued.
        '*
        Const BG_E_HTTP_ERROR_100 As Int64 = &H80190064

        '*
        '* MessageId: BG_E_HTTP_ERROR_101
        '*
        '* MessageText:
        '*
        '*  The server has switched protocols in an upgrade header.
        '*
        Const BG_E_HTTP_ERROR_101 As Int64 = &H80190065

        '*
        '* MessageId: BG_E_HTTP_ERROR_200
        '*
        '* MessageText:
        '*
        '*  The request completed successfully.
        '*
        Const BG_E_HTTP_ERROR_200 As Int64 = &H801900C8

        '*
        '* MessageId: BG_E_HTTP_ERROR_201
        '*
        '* MessageText:
        '*
        '*  The request has been fulfilled and resulted in the creation of a new resource.
        '*
        Const BG_E_HTTP_ERROR_201 As Int64 = &H801900C9

        '*
        '* MessageId: BG_E_HTTP_ERROR_202
        '*
        '* MessageText:
        '*
        '*  The request has been accepted for processing, but the processing has not been completed.
        '*
        Const BG_E_HTTP_ERROR_202 As Int64 = &H801900CA

        '*
        '* MessageId: BG_E_HTTP_ERROR_203
        '*
        '* MessageText:
        '*
        '*  The returned meta information in the entity-header is not the definitive set available from the origin server.
        '*
        Const BG_E_HTTP_ERROR_203 As Int64 = &H801900CB

        '*
        '* MessageId: BG_E_HTTP_ERROR_204
        '*
        '* MessageText:
        '*
        '*  The server has fulfilled the request, but there is no new information to send back.
        '*
        Const BG_E_HTTP_ERROR_204 As Int64 = &H801900CC

        '*
        '* MessageId: BG_E_HTTP_ERROR_205
        '*
        '* MessageText:
        '*
        '*  The request has been completed, and the client program should reset the document view that caused the request to be sent to allow the user to easily initiate another input action.
        '*
        Const BG_E_HTTP_ERROR_205 As Int64 = &H801900CD

        '*
        '* MessageId: BG_E_HTTP_ERROR_206
        '*
        '* MessageText:
        '*
        '*  The server has fulfilled the partial GET request for the resource.
        '*
        Const BG_E_HTTP_ERROR_206 As Int64 = &H801900CE

        '*
        '* MessageId: BG_E_HTTP_ERROR_300
        '*
        '* MessageText:
        '*
        '*  The server couldn't decide what to return.
        '*
        Const BG_E_HTTP_ERROR_300 As Int64 = &H8019012C

        '*
        '* MessageId: BG_E_HTTP_ERROR_301
        '*
        '* MessageText:
        '*
        '*  The requested resource has been assigned to a new permanent URI (Uniform Resource Identifier), and any future references to this resource should be done using one of the returned URIs.
        '*
        Const BG_E_HTTP_ERROR_301 As Int64 = &H8019012D

        '*
        '* MessageId: BG_E_HTTP_ERROR_302
        '*
        '* MessageText:
        '*
        '*  The requested resource resides temporarily under a different URI (Uniform Resource Identifier).
        '*
        Const BG_E_HTTP_ERROR_302 As Int64 = &H8019012E

        '*
        '* MessageId: BG_E_HTTP_ERROR_303
        '*
        '* MessageText:
        '*
        '*  The response to the request can be found under a different URI (Uniform Resource Identifier) and should be retrieved using a GET method on that resource.
        '*
        Const BG_E_HTTP_ERROR_303 As Int64 = &H8019012F

        '*
        '* MessageId: BG_E_HTTP_ERROR_304
        '*
        '* MessageText:
        '*
        '*  The requested resource has not been modified.
        '*
        Const BG_E_HTTP_ERROR_304 As Int64 = &H80190130

        '*
        '* MessageId: BG_E_HTTP_ERROR_305
        '*
        '* MessageText:
        '*
        '*  The requested resource must be accessed through the proxy given by the location field.
        '*
        Const BG_E_HTTP_ERROR_305 As Int64 = &H80190131

        '*
        '* MessageId: BG_E_HTTP_ERROR_307
        '*
        '* MessageText:
        '*
        '*  The redirected request keeps the same verb. HTTP/1.1 behavior.
        '*
        Const BG_E_HTTP_ERROR_307 As Int64 = &H80190133

        '*
        '* MessageId: BG_E_HTTP_ERROR_400
        '*
        '* MessageText:
        '*
        '*  The request could not be processed by the server due to invalid syntax.
        '*
        Const BG_E_HTTP_ERROR_400 As Int64 = &H80190190

        '*
        '* MessageId: BG_E_HTTP_ERROR_401
        '*
        '* MessageText:
        '*
        '*  The requested resource requires user authentication.
        '*
        Const BG_E_HTTP_ERROR_401 As Int64 = &H80190191

        '*
        '* MessageId: BG_E_HTTP_ERROR_402
        '*
        '* MessageText:
        '*
        '*  Not currently implemented in the HTTP protocol.
        '*
        Const BG_E_HTTP_ERROR_402 As Int64 = &H80190192

        '*
        '* MessageId: BG_E_HTTP_ERROR_403
        '*
        '* MessageText:
        '*
        '*  The server understood the request, but is refusing to fulfill it.
        '*
        Const BG_E_HTTP_ERROR_403 As Int64 = &H80190193

        '*
        '* MessageId: BG_E_HTTP_ERROR_404
        '*
        '* MessageText:
        '*
        '*  The server has not found anything matching the requested URI (Uniform Resource Identifier).
        '*
        Const BG_E_HTTP_ERROR_404 As Int64 = &H80190194

        '*
        '* MessageId: BG_E_HTTP_ERROR_405
        '*
        '* MessageText:
        '*
        '*  The method used is not allowed.
        '*
        Const BG_E_HTTP_ERROR_405 As Int64 = &H80190195

        '*
        '* MessageId: BG_E_HTTP_ERROR_406
        '*
        '* MessageText:
        '*
        '*  No responses acceptable to the client were found.
        '*
        Const BG_E_HTTP_ERROR_406 As Int64 = &H80190196

        '*
        '* MessageId: BG_E_HTTP_ERROR_407
        '*
        '* MessageText:
        '*
        '*  Proxy authentication required.
        '*
        Const BG_E_HTTP_ERROR_407 As Int64 = &H80190197

        '*
        '* MessageId: BG_E_HTTP_ERROR_408
        '*
        '* MessageText:
        '*
        '*  The server timed out waiting for the request.
        '*
        Const BG_E_HTTP_ERROR_408 As Int64 = &H80190198

        '*
        '* MessageId: BG_E_HTTP_ERROR_409
        '*
        '* MessageText:
        '*
        '*  The request could not be completed due to a conflict with the current state of the resource. The user should resubmit with more information.
        '*
        Const BG_E_HTTP_ERROR_409 As Int64 = &H80190199

        '*
        '* MessageId: BG_E_HTTP_ERROR_410
        '*
        '* MessageText:
        '*
        '*  The requested resource is no longer available at the server, and no forwarding address is known.
        '*
        Const BG_E_HTTP_ERROR_410 As Int64 = &H8019019A

        '*
        '* MessageId: BG_E_HTTP_ERROR_411
        '*
        '* MessageText:
        '*
        '*  The server refuses to accept the request without a defined content length.
        '*
        Const BG_E_HTTP_ERROR_411 As Int64 = &H8019019B

        '*
        '* MessageId: BG_E_HTTP_ERROR_412
        '*
        '* MessageText:
        '*
        '*  The precondition given in one or more of the request header fields evaluated to false when it was tested on the server.
        '*
        Const BG_E_HTTP_ERROR_412 As Int64 = &H8019019C

        '*
        '* MessageId: BG_E_HTTP_ERROR_413
        '*
        '* MessageText:
        '*
        '*  The server is refusing to process a request because the request entity is larger than the server is willing or able to process.
        '*
        Const BG_E_HTTP_ERROR_413 As Int64 = &H8019019D

        '*
        '* MessageId: BG_E_HTTP_ERROR_414
        '*
        '* MessageText:
        '*
        '*  The server is refusing to service the request because the request URI (Uniform Resource Identifier) is longer than the server is willing to interpret.
        '*
        Const BG_E_HTTP_ERROR_414 As Int64 = &H8019019E

        '*
        '* MessageId: BG_E_HTTP_ERROR_415
        '*
        '* MessageText:
        '*
        '*  The server is refusing to service the request because the entity of the request is in a format not supported by the requested resource for the requested method.
        '*
        Const BG_E_HTTP_ERROR_415 As Int64 = &H8019019F

        '*
        '* MessageId: BG_E_HTTP_ERROR_416
        '*
        '* MessageText:
        '*
        '*  The server could not satisfy the range request.
        '*
        Const BG_E_HTTP_ERROR_416 As Int64 = &H801901A0

        '*
        '* MessageId: BG_E_HTTP_ERROR_417
        '*
        '* MessageText:
        '*
        '*  The expectation given in an Expect request-header field could not be met by this server.
        '*
        Const BG_E_HTTP_ERROR_417 As Int64 = &H801901A1

        '*
        '* MessageId: BG_E_HTTP_ERROR_449
        '*
        '* MessageText:
        '*
        '*  The request should be retried after doing the appropriate action.
        '*
        Const BG_E_HTTP_ERROR_449 As Int64 = &H801901C1

        '*
        '* MessageId: BG_E_HTTP_ERROR_500
        '*
        '* MessageText:
        '*
        '*  The server encountered an unexpected condition that prevented it from fulfilling the request.
        '*
        Const BG_E_HTTP_ERROR_500 As Int64 = &H801901F4

        '*
        '* MessageId: BG_E_HTTP_ERROR_501
        '*
        '* MessageText:
        '*
        '*  The server does not support the functionality required to fulfill the request.
        '*
        Const BG_E_HTTP_ERROR_501 As Int64 = &H801901F5

        '*
        '* MessageId: BG_E_HTTP_ERROR_502
        '*
        '* MessageText:
        '*
        '*  The server, while acting as a gateway or proxy, received an invalid response from the upstream server it accessed in attempting to fulfill the request.
        '*
        Const BG_E_HTTP_ERROR_502 As Int64 = &H801901F6

        '*
        '* MessageId: BG_E_HTTP_ERROR_503
        '*
        '* MessageText:
        '*
        '*  The service is temporarily overloaded.
        '*
        Const BG_E_HTTP_ERROR_503 As Int64 = &H801901F7

        '*
        '* MessageId: BG_E_HTTP_ERROR_504
        '*
        '* MessageText:
        '*
        '*  The request was timed out waiting for a gateway.
        '*
        Const BG_E_HTTP_ERROR_504 As Int64 = &H801901F8

        '*
        '* MessageId: BG_E_HTTP_ERROR_505
        '*
        '* MessageText:
        '*
        '*  The server does not support, or refuses to support, the HTTP protocol version that was used in the request message.
        '*
        Const BG_E_HTTP_ERROR_505 As Int64 = &H801901F9

    End Class
End Namespace


