# tshock-prometheus

TShock Plugin that generates prometheus stats

Exposes a server on port `9763` by default that can be scraped by prometheus.
(the endpoint is `/metrics`)

## Installation

### TShock

Copy `TShockPrometheus.dll` and it's dependencies into your `ServerPlugins`
directory. These are all the files contained in the releases archive.
