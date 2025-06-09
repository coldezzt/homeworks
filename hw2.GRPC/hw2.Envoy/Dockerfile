FROM envoyproxy/envoy:v1.33-latest
COPY envoy.yaml /etc/envoy/envoy.yaml
EXPOSE 1400 9901
CMD ["envoy", "-c", "/etc/envoy/envoy.yaml", "--log-level", "debug"]

# docker run -d -p 1400:1400 -p 9901:9901 --name envoy-proxy my-envoy-grpc