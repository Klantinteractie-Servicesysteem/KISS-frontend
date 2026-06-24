docker run --rm `
  -v "${PSScriptRoot}/crawler.yml:/home/app/config/crawler.yml:ro" `
  docker.elastic.co/integrations/crawler:1.0.0 `
  -c "/home/app/bin/crawler crawl /home/app/config/crawler.yml"
