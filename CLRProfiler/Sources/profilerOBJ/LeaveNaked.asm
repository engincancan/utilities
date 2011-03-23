extern LeaveStub:proc
extern TailcallStub:proc

;typedef void LeaveNaked2(
;         rcx =  FunctionID funcId, 
;         rdx =  UINT_PTR clientData, 
;         r8  =  COR_PRF_FRAME_INFO func, 
;         r9  =  COR_PRF_FUNCTION_ARGUMENT_RANGE *retvalRange);
_TEXT segment para 'CODE'

        align   16

        public  LeaveNaked2

LeaveNaked2    proc    frame

        ; save integer return register
        push    rax
        .allocstack 8

        sub     rsp, 20h

        .endprolog

        call    LeaveStub

        add     rsp, 20h

        ; restore integer return register
        pop                     rax

        ; return
        ret

LeaveNaked2    endp

;typedef void TailcallNaked2(
;         rcx =  FunctionID funcId, 
;         rdx =  UINT_PTR clientData, 
;         t8  =  COR_PRF_FRAME_INFO,

        align   16

        public  TailcallNaked2

TailcallNaked2   proc    frame

        ; save rax
        push    rax
        .allocstack 8

        sub     rsp, 20h

        .endprolog

        call    TailcallStub

        add     rsp, 20h

        ; restore rax
        pop     rax

        ; return
        ret

TailcallNaked2   endp

_TEXT ends

end