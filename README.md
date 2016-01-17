This project demonstrates some common network utilities implementation written on raw sockets.

1. Meleagre.Network. Common library providing partial IP and ICMP protocols implementation.
2. MPing - ping utility based on ICMP protocol. Sends echo-messages and waits for replies. 
3. MTracert - traceroute utility, displays the netowrk path to specified host.
4. MTU discovery - utility that tries to determine MTU (maximum tranmission unit) to specifed host.

Starting any of these utilities without parameters shows help.

NOTE! Working with raw sockets requires Administrator privileges, so all three utilites should be started 
from cmd with elevated rights. 
