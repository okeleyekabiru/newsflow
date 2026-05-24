FROM nginx:alpine

# Copy static files
COPY NewsFlowApp.html /usr/share/nginx/html/index.html
COPY pages/           /usr/share/nginx/html/pages/
COPY components/      /usr/share/nginx/html/components/

# Custom nginx config — SPA-friendly: all unmatched routes serve index.html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 3000
