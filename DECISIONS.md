## Swagger 环境策略
Swagger UI 仅在 Development 环境启用（ASPNETCORE_ENVIRONMENT=Development），
不在 Production 环境暴露 API 文档，符合生产安全最佳实践。
如需在容器中测试 Swagger，可在 docker-compose.yml 中临时将
ASPNETCORE_ENVIRONMENT 设为 Development。