addi $10 zero 3 ; set register 3 to 10 for the loop comparison

.loop
    addi $1 4 4 ; increment 
    cmp 4 3
    jne loop ; if register 4 does not equal register 3, jump back

; prove we left the loop
addi $200 zero 7