import urllib2, urllib
import time

url = 'http://localhost:2083/key'
data = urllib.urlencode({
	'spec' : 't,e,s, t, s-left/50:4/100, c-c/50, c-v/100, c-v'
	})
time.sleep(3) # gives you 2 seconds to Alt+Tab back over to Notepad before executing the key action
request = urllib2.Request(url, data)
response = urllib2.urlopen(request)
d = response.read()
print "Response:", d


