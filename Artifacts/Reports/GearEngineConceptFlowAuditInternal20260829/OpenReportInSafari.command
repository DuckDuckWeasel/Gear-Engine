#!/bin/zsh
set -e
report_directory="${0:A:h}"
report_port=4173
while /usr/sbin/lsof -nP -iTCP:"$report_port" -sTCP:LISTEN >/dev/null 2>&1; do
  report_port=$((report_port + 1))
done
cd -- "$report_directory"
python3 -m http.server "$report_port" --bind 127.0.0.1 >/tmp/interactive-report-server.log 2>&1 &
report_server_pid=$!
trap 'kill "$report_server_pid" >/dev/null 2>&1 || true' EXIT INT TERM
open -a Safari "http://127.0.0.1:$report_port/"
wait "$report_server_pid"
