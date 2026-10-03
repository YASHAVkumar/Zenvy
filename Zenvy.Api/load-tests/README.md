# API load tests (k6)

Install [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/), start the API, then run:

```powershell
k6 run -e BASE_URL=http://localhost:5000 -e ACCESS_TOKEN=<jwt> load-tests/api-load.js
```

`ACCESS_TOKEN` is optional. Without it, the script load-tests only the public root endpoint. With it, it also tests the authorized categories endpoint.

The default profile ramps to 10 virtual users, holds for one minute, and ramps down. Override it with `-e VUS=25`. The run fails when more than 1% of requests fail or p95 latency exceeds 750 ms.
