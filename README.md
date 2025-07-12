# ScholarMCP

## 项目简介 / Project Introduction / プロジェクト紹介

- ScholarMCP 是一个基于 Next.js 前端、.NET/ASP.NET 后端、Neo4j 和 PostgreSQL 数据库的多层架构项目。
- ScholarMCP is a multi-layer architecture project with Next.js frontend, .NET/ASP.NET backend, Neo4j and PostgreSQL databases.
- ScholarMCPはNext.jsフロントエンド、.NET/ASP.NETバックエンド、Neo4jとPostgreSQLデータベースを備えた多層アーキテクチャプロジェクトです。

## 目录结构 / Structure / 構成

- `frontend/` 前端 (Next.js)
- `backend/` 后端 (.NET/ASP.NET, MCP, Agent)
- `database/` 数据库相关 (PostgreSQL, Neo4j)
- `docker/` Docker 配置
- `rawfile/` 用户上传文件
- `.cursor/rule/` 规则文件

## 快速开始 / Quick Start / クイックスタート

1. 克隆仓库 / Clone the repo / リポジトリをクローン
2. 安装依赖 / Install dependencies / 依存関係をインストール
3. 启动服务 / Start services / サービスを起動

## Docker 部署 / Docker Deployment / Dockerデプロイ

1. 构建并启动所有服务：
   ```sh
   docker-compose up --build
   ```
2. 后端服务端口：5000，PostgreSQL：5432，Neo4j：7474/7687
3. 可通过 docker-compose.yml 和各 Dockerfile 自定义配置。

1. Build and start all services:
   ```sh
   docker-compose up --build
   ```
2. Backend port: 5000, PostgreSQL: 5432, Neo4j: 7474/7687
3. You can customize config via docker-compose.yml and Dockerfiles.

1. すべてのサービスをビルド・起動：
   ```sh
   docker-compose up --build
   ```
2. バックエンド: 5000, PostgreSQL: 5432, Neo4j: 7474/7687
3. docker-compose.ymlや各Dockerfileで設定をカスタマイズ可能。

## API 说明 / API Description / API説明

- RESTful API: `/api/rest`
- MCP API: `/api/mcp`
- Agent API: `/api/agent`

## 许可证 / License / ライセンス

MIT
