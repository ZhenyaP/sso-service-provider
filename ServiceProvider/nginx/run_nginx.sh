#!/bin/bash

echo "Running run_nginx.sh at $(date)"
echo "Substituting environment-specific values to nginx.conf file"
sed -i "s|\$sed\.NGINX_SERVER_NAME|$NGINX_SERVER_NAME|g" /etc/nginx/nginx.conf