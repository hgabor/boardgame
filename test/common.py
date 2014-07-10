import json
import urllib.error
import urllib.request

__author__ = 'hali'


urlbase = 'http://localhost:8080'

def do_request(page, data=None, key=None):
    """
    :param page:
    :param data:
    :param key:
    :return: data
    :rtype: dict
    """
    url = urlbase + page
    headers = {'Content-Type': 'application/json'}
    if key is not None:
        headers['BoardApiKey'] = key
    if data is None:
        data_bytes = None
    else:
        data_bytes = json.dumps(data).encode('utf-8')
    req = urllib.request.Request(url, data=data_bytes, headers=headers)
    try:
        with urllib.request.urlopen(req) as f:
            return_data = f.read().decode('utf-8')
            if f.status != 200:
                raise Exception(return_data)
            return json.loads(return_data)
    except urllib.error.HTTPError as e:
        return_data = e.read().decode('utf-8')
        return json.loads(return_data)

