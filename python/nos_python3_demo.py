import base64
import hashlib
import hmac
from email.utils import formatdate

import requests


def calc_auth_header(access_key, secret_key, http_method, content_md5, content_type, date,
                     canonicalized_headers, canonicalized_resource):
    str_to_sign = '%s\n%s\n%s\n%s\n' % (
        http_method,
        content_md5,
        content_type,
        date
    )
    canonicalized_headers = {k.lower(): v for (k, v) in canonicalized_headers.items()}
    canonicalized_headers = {k: canonicalized_headers[k] for k in sorted(canonicalized_headers.keys())}

    for k, v in canonicalized_headers:
        str_to_sign += '%s:%s\n' % (k, v)

    str_to_sign += "%s" % (
        canonicalized_resource
    )

    hmac_sha256 = hmac.new(bytes(secret_key, encoding='utf8'), bytes(str_to_sign, encoding='utf8'), hashlib.sha256)
    signature = base64.encodebytes(hmac_sha256.digest()).decode('utf8').rstrip()

    return 'NOS %s:%s' % (
        access_key, signature
    )


def put_object():
    endpoint = 'nos endpoint'
    access_key = 'your accessKey'
    secret_key = 'your secretKey'
    bucket_name = 'your bucketName'
    object_name = 'your objectName'
    file_name = 'file name'

    date_rfc1123 = formatdate(timeval=None, localtime=False, usegmt=True)
    content_type = 'image/png'

    with open(file_name, "rb") as f:
        data = f.read()

    hash_md5 = hashlib.md5()
    with open(file_name, "rb") as f:
        for chunk in iter(lambda: f.read(4096), b""):
            hash_md5.update(chunk)

    content_md5 = hash_md5.hexdigest()
    authorization = calc_auth_header(access_key, secret_key, 'PUT', content_md5, content_type, date_rfc1123, {},
                                     '/' + bucket_name + '/' + object_name)
    headers = {'date': date_rfc1123,
               'content-type': content_type,
               'content-md5': content_md5,
               'authorization': authorization,
               }
    r = requests.put('http://%s.%s/%s' % (
        bucket_name, endpoint, object_name
    ), headers=headers, data=data)
    print(r.status_code)
    print(r.text)


put_object()
