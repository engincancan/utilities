;==========================
;Initialise
;==========================
#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
SendMode Input  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.
SetTitleMatchMode 2

capslock:: send, {Escape}

;Map to left arrow
;LWIN & ^h:: send, {left} 

;Map to right arrow
;LWIN & ^l:: send, {right} 

;Map to up arrow
;LWIN & ^k:: send, {up}

;Map to down arrow
;LWIN & ^j:: send, {down}

;Stop capslock & key from toggling capslock.
;*capslock::
;+capslock::
;capslock & down:: 
;capslock & up::
;capslock & right::
;capslock & left::
;capslock & appskey::
;capslock & tab::
;capslock & `::
;capslock & 3:: 
;capslock & 5::
;capslock & 7::
;capslock & 8::
;capslock & 9::
;capslock & 0::
;capslock & -::
;capslock & =::
;capslock & y::
;capslock & a::
;capslock & s::
;capslock & `;::
;capslock & \::
;capslock & z::
;capslock & v::
;capslock & p:: 
;capslock & ,::
;capslock & .::
;capslock & space::
;capslock & enter::
;capslock & w::
;capslock & [::
;capslock & ]::
;capslock & #::
