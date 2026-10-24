# Lifeline
Create a link between 2 websites

## Docker (C#) requirements
- Copy CSharp/docker-compose.yml in your server
- Make a data folder
- Inside data/, create a config.json file (see config.json.example)
- Go back at the root, run `docker compose up
- Add the config to your web server (example with nginx)
```nginx
location /lifeline/ {
    proxy_pass http://localhost:6842;
}
```

## PHP Requirements
PHP modules:
- sqlite3
- curl

## Creating a new lifeline

Create a file named `config.json` at the root (you can take config.json.example as an example), for each lifeline it need to contains the following information:
- name: Name of the receipient of the lifeline, this is purely indicative
- token: Unique identifier, both you and you recipient need to have the same one
- target: Location of the `receive.php` file of your recipient
- path: (optional) additional payload you can send through the lifeline