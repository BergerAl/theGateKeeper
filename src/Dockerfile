FROM node:18-alpine AS builder

RUN mkdir /usr/app

WORKDIR /usr/app

COPY ./package.json ./
RUN npm install
COPY . .

RUN npm run build


FROM nginx:alpine

COPY ./nginx/conf/nginx.conf /etc/nginx/conf.d/default.conf

WORKDIR /usr/share/nginx/html

RUN rm -rf ./*

COPY --from=builder /usr/app/build .
EXPOSE 80
# ENTRYPOINT [ "nginx" "-g", "daemon off;" ]
