addi 10 zero 2 ; set register 2 to 10 for the loop comparison

.loop
    addi 1 3 3 ; increment 
    cmp 3 2
    jne loop ; if register 3 does not equal register 2, jump back

; prove we left the loop
addi 200 zero 5