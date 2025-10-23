import http.server
import socketserver
import os

PORT = 8000

WEB_DIR = os.path.join(os.path.dirname(__file__), 'wwwroot')
os.chdir(WEB_DIR)  # Change working directory to serve files from wwwroot

Handler = http.server.SimpleHTTPRequestHandler
with socketserver.TCPServer(("", PORT), Handler) as httpd:
    print("Serving from", WEB_DIR, "at port", PORT)
    httpd.serve_forever()