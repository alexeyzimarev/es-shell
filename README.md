# Event Store Shell

Try it out!

Start Event Store using Docker Compose (use `docker-compose.yml` from this repository):

```bash
docker-compose up
```

Run the tool image:

```bash
docker run --network "host" --rm -ti azimarev/es-shell
```

Connect to the instance:

```
connect
```

## Commands

`connect` - connect to a single-node instance

Options:
`--user`
`--password`
`--host`

`streams` - get the list of streams (max 10 now!)

`stream set <name>` - set the current stream scope

`stream read` - read from the current stream

Options:
`--start`
`--count`

When reading the stream, the shell will keep the last page event number.
When you read from the stream again, it will read from the stored position, forward.

`stream meta` - read the stream metadata

`exit` - leave