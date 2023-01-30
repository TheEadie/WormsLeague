## How to refresh the certificate used


- Generate a new cert using certbot

```
sudo mkdir /etc/letsencrypt
sudo mkdir /var/lib/letsencrypt
sudo docker run -it --rm --name certbot \
                  -v "/etc/letsencrypt:/etc/letsencrypt" \
                  -v "/var/lib/letsencrypt:/var/lib/letsencrypt" \
                  certbot/certbot certonly --manual --preferred-challenges dns
```

- Follow the prompts to give an email address, domain name etc
- When ask copy the TXT challenge value into cloudflare and then press enter

- You should have two files generated:
    - fullchain.pem
    - privkey.pem

- Create a pfx file

```
sudo openssl pkcs12 -export -out /etc/letsencrypt/live/worms.davideadie.dev/azure.pfx -inkey /etc/letsencrypt/live/worms.davideadie.dev/privkey.pem -in /etc/letsencrypt/live/worms.davideadie.dev/fullchain.pem -keypbe PBE-SHA1-3DES -certpbe PBE-SHA1-3DES -macalg SHA1
```

(3DES-SHA1 should be changed to a better encryption once ACA [supports it](https://github.com/microsoft/azure-container-apps/issues/511))

- Copy the file locally

```
sudo cp /etc/letsencrypt/live/worms.davideadie.dev/azure.pfx /mnt/c/
```

- Navigate to the Azure Container App
- Click Custom Domains
- Add the Certificate to the custom domain
