# The GateKeeper

Is an application for my friends and I where we can compare each other. The aim is to be above the chosen gatekeeper, which will be selected randomly. There is no intend on harassing people, that are below the gatekeeper, it's just there to motivate people. Furthermore there will be a voting system, but it's not decided what it will be. Probably something, that the person has to stream their game to the friends and figure out what could have done better.

## Docker

### Development

`docker-compose.override.yml` is automatically applied on top of `docker-compose.yml` by Docker Compose.

```bash
docker compose up -d
```

### Production (Raspberry Pi)

`docker-compose.prod.yml` must be explicitly provided.

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```
