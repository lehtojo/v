.section .text
.intel_syntax noprefix
.file 1 "Sandbox.v"
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

_V1axxxxxx_rx:
add rcx, rdx
add rcx, r8
lea rax, [rcx+r9]
add rax, [rsp+40]
add rax, [rsp+48]
ret

.global _V1xxx_rx
_V1xxx_rx:
push rbx
sub rsp, 48
lea r8, [rcx+1]
mov rbx, rcx
sar rbx, 1
sal rcx, 2
lea r10, [rdx+1]
mov r11, rdx
sal r11, 1
sar rdx, 2
mov qword ptr [rsp+40], rdx
mov r9, r10
mov qword ptr [rsp+32], r11
mov rdx, rbx
xchg r8, rcx
call _V1axxxxxx_rx
add rsp, 48
pop rbx
ret

_V1bxxxxxxiscd_rd:
add rcx, rdx
add rcx, r8
add rcx, r9
add rcx, [rsp+40]
add rcx, [rsp+48]
add rcx, [rsp+56]
add rcx, [rsp+64]
add rcx, [rsp+72]
cvtsi2sd xmm1, rcx
addsd xmm1, xmm0
movsd xmm0, xmm1
ret

.global _V1yxx_rd
_V1yxx_rd:
push rbx
push rsi
push rdi
push rbp
sub rsp, 72
mov r8, rcx
sub r8, 3
mov r9, rcx
sub r9, 2
mov r10, rcx
sub r10, 1
lea r11, [rcx+1]
lea rbx, [rcx+2]
lea rsi, [rcx+3]
mov rdi, rcx
imul rdi, 42
lea rbp, [rcx*2+rcx]
imul rcx, -1
cvtsi2sd xmm0, rdx
movsd xmm1, qword ptr [rip+_V1yxx_rd_C0]
addsd xmm0, xmm1
mov qword ptr [rsp+64], rcx
mov rdx, r9
mov rcx, r8
mov r8, r10
mov r9, r11
mov qword ptr [rsp+32], rbx
mov qword ptr [rsp+40], rsi
mov qword ptr [rsp+48], rdi
mov qword ptr [rsp+56], rbp
call _V1bxxxxxxiscd_rd
add rsp, 72
pop rbp
pop rdi
pop rsi
pop rbx
ret

_V4initv_rx:
sub rsp, 40
mov rcx, 1
mov rdx, 1
call _V1xxx_rx
xor rcx, rcx
xor rdx, rdx
call _V1yxx_rd
mov rax, 1
add rsp, 40
ret

_V8allocatex_rPh:
push rbx
push rsi
sub rsp, 40
mov r8, [rip+_VN10Allocation_current]
test r8, r8
je _V8allocatex_rPh_L0
mov rdx, [r8+16]
lea r9, [rdx+rcx]
cmp r9, 1000000
jg _V8allocatex_rPh_L0
lea r9, [rdx+rcx]
mov qword ptr [r8+16], r9
lea r9, [rdx+rcx]
mov rax, [r8+8]
add rax, rdx
add rsp, 40
pop rsi
pop rbx
ret
_V8allocatex_rPh_L0:
mov rbx, rcx
mov rcx, 1000000
call _V17internal_allocatex_rPh
mov rcx, 24
mov rsi, rax
call _V17internal_allocatex_rPh
mov qword ptr [rax+8], rsi
mov qword ptr [rax+16], rbx
mov qword ptr [rip+_VN10Allocation_current], rax
mov rax, rsi
add rsp, 40
pop rsi
pop rbx
ret

_V8inheritsPhS__rx:
push rbx
push rsi
mov r8, [rcx]
mov r9, [rdx]
movzx r10, byte ptr [r9]
xor rax, rax
_V8inheritsPhS__rx_L1:
_V8inheritsPhS__rx_L0:
movzx rcx, byte ptr [r8+rax]
add rax, 1
cmp rcx, r10
jnz _V8inheritsPhS__rx_L4
mov r11, rcx
mov rbx, 1
_V8inheritsPhS__rx_L7:
_V8inheritsPhS__rx_L6:
movzx r11, byte ptr [r8+rax]
movzx rsi, byte ptr [r9+rbx]
add rax, 1
add rbx, 1
cmp r11, rsi
jz _V8inheritsPhS__rx_L9
cmp r11, 1
jne _V8inheritsPhS__rx_L9
test rsi, rsi
jne _V8inheritsPhS__rx_L9
mov rax, 1
pop rsi
pop rbx
ret
_V8inheritsPhS__rx_L9:
jmp _V8inheritsPhS__rx_L6
_V8inheritsPhS__rx_L8:
jmp _V8inheritsPhS__rx_L3
_V8inheritsPhS__rx_L4:
cmp rcx, 2
jne _V8inheritsPhS__rx_L3
xor rax, rax
pop rsi
pop rbx
ret
_V8inheritsPhS__rx_L3:
jmp _V8inheritsPhS__rx_L0
_V8inheritsPhS__rx_L2:
pop rsi
pop rbx
ret

.section .data

_VN10Allocation_current:
.quad 0

_VN4Page_configuration:
.quad _VN4Page_descriptor

_VN4Page_descriptor:
.quad _VN4Page_descriptor_0
.long 24
.long 0

_VN4Page_descriptor_0:
.ascii "Page"
.byte 0
.byte 1
.byte 2
.byte 0

_VN10Allocation_configuration:
.quad _VN10Allocation_descriptor

_VN10Allocation_descriptor:
.quad _VN10Allocation_descriptor_0
.long 8
.long 0

_VN10Allocation_descriptor_0:
.ascii "Allocation"
.byte 0
.byte 1
.byte 2
.byte 0

.balign 16
_V1yxx_rd_C0:
.byte 57, 180, 200, 118, 190, 159, 246, 63 # 1.414