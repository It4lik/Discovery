FROM alpine

RUN apk update \
	&& apk add openssh

COPY ./entrypoint.sshserver.sh /entrypoint.sh

ENTRYPOINT /entrypoint.sh
