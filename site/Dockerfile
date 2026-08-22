FROM hugomods/hugo:debian-dart-sass-base AS build
WORKDIR /src

ARG GIT_COMMIT_SHA_SHORT=unknown
ENV GIT_COMMIT_SHA_SHORT=${GIT_COMMIT_SHA_SHORT}

COPY . .
RUN hugo --minify

FROM nginx:alpine AS final
COPY --from=build /src/public /usr/share/nginx/html

EXPOSE 80
