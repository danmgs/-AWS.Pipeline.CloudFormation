#!/bin/bash

for i in `seq 1 10`;
do
  HTTP_CODE=`curl -s -o /dev/null -I -w '%{http_code}' http://localhost/Home/Health`
  if [ "$HTTP_CODE" == "200" ]; then
    echo "Successfully pulled page."
    exit 0;
  fi
  echo "Attempt to curl endpoint returned HTTP Code $HTTP_CODE. Backing off and retrying."
  sleep 10
done
echo "Server did not come up after expected time. Failing."
exit 1
