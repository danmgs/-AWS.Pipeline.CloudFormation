#!/bin/bash

## Create a reverse-proxy config file for Apache http service.

if [ -f /etc/httpd/conf.d/default-site.conf ]; then
    rm -f /etc/httpd/conf.d/default-site.conf
    echo "delete"
fi

cat <<EOF >> /etc/httpd/conf.d/default-site.conf
 <VirtualHost *:80>
  DocumentRoot /usr/app/wwwroot
  <Directory "/usr/app/wwwroot">
    AllowOverride None
    Require all granted
  </Directory>
  ProxyPass / http://127.0.0.1:5000/
  ProxyPassReverse / http://127.0.0.1:5000/


  ProxyPass /lib http://127.0.0.1:5000/
  ProxyPassReverse /lib http://127.0.0.1:5000/

  ProxyPass /css http://127.0.0.1:5000/
  ProxyPassReverse /css http://127.0.0.1:5000/

  ProxyPass /js http://127.0.0.1:5000/js
  ProxyPassReverse /js http://127.0.0.1:5000/

</VirtualHost>
EOF

echo "file created"
