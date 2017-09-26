netsh wlan stop hostednetwork >NUL
netsh wlan set hostednetwork mode=allow ssid=RND key=noa-labs.com
netsh wlan start hostednetwork
netsh wlan show hostednetwork setting=security
netsh wlan start hostednetwork