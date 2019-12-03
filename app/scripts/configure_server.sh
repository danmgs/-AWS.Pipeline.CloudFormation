#!/bin/bash

## Create a reverse-proxy config file for Apache http service.

if [ -f /etc/httpd/conf.d/default-site.conf ]; then
    rm -f /etc/httpd/conf.d/default-site.conf
    echo "- file deleted /etc/httpd/conf.d/default-site.conf"
fi

cat <<EOF >> /etc/httpd/conf.d/default-site.conf
<VirtualHost *:80>
  ProxyPass / http://127.0.0.1:5000/
  ProxyPassReverse / http://127.0.0.1:5000/
</VirtualHost>
EOF

echo "- file created /etc/httpd/conf.d/default-site.conf"
