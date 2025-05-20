import json
import base64
import hashlib
import hmac
import http.server
from datetime import datetime, timedelta

from urllib.parse import parse_qs
# WARNING: In production, store a signing key in Azure Key Vault
SECRET = b"sample-secret-key"

class LicenseHandler(http.server.BaseHTTPRequestHandler):
    def _write_json(self, obj, status=200):
        data = json.dumps(obj).encode()
        self.send_response(status)
        self.send_header('Content-Type', 'application/json')
        self.send_header('Content-Length', str(len(data)))
        self.end_headers()
        self.wfile.write(data)

    def do_GET(self):
        if self.path.startswith('/validate'):
            self._write_json({'status': 'valid'})
        else:
            self.send_error(404)

    def do_POST(self):
        if self.path.startswith('/offlineToken'):
            length = int(self.headers.get('Content-Length', 0))
            body = self.rfile.read(length).decode()
            params = parse_qs(body)
            license_id = params.get('license', [''])[0]
            expiry = datetime.utcnow() + timedelta(days=7)
            payload = f"{license_id}:{int(expiry.timestamp())}"
            signature = hmac.new(SECRET, payload.encode(), hashlib.sha256).digest()
            token = base64.urlsafe_b64encode(payload.encode() + b'.' + signature).decode()
            self._write_json({'token': token, 'expires': expiry.isoformat()})
        else:
            self.send_error(404)

if __name__ == '__main__':
    server = http.server.HTTPServer(('0.0.0.0', 7071), LicenseHandler)
    print('License service listening on port 7071')
    server.serve_forever()
