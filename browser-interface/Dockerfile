FROM node:16.8-alpine
RUN apk update \
    && apk --no-cache --update add build-base

COPY init.sh /init.sh
RUN chmod +x /init.sh
