#!/bin/sh

adduser john -D
passwd john -d
ssh-keygen -A
sed -i 's/#PermitEmptyPasswords no/PermitEmptyPasswords yes/g' /etc/ssh/sshd_config
/usr/sbin/sshd

sleep 999999

