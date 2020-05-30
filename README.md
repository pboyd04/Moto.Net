# Moto.Net
A C# client to handle MotoTrbo including native wire line communication, LRRP, and TMS

# MotoMond
This service logs radios and calls. It also supports the following commands:

1. check <radio id> - Issues a radio check to the specified radio via the IP connected repeater
2. locate <radio id> - Sends an LRRP request to the specified radio via a USB connected control radio
3. test <radio id> <message> - Sends a text message to the specified radio via a USB connected control radio
4. getip <radio id> - Gets the IP address of the specified radio
