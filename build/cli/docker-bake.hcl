variable "IMAGE_NAME" { default = "theeadie/worms-cli" }
variable "DOCKERFILE_PATH" { default = "build/cli/Dockerfile" }
variable "VERSION" {}

target "package" {
  inherits = ["base"]
  tags = [
      "${IMAGE_NAME}:latest",
      "${IMAGE_NAME}:${VERSION}",
      "${IMAGE_NAME}:${split(".", "${VERSION}")[0]}",
      "${IMAGE_NAME}:${split(".", "${VERSION}")[0]}.${split(".", "${VERSION}")[1]}",
  ]
  cache-from = [
      "type=gha,scope=package-${IMAGE_NAME}",
      "type=gha,scope=build-${IMAGE_NAME}"
  ]
  cache-to = [
      "type=gha,mode=max,scope=package-${IMAGE_NAME}"
  ]
}

target "base" {
  context = "."
  dockerfile = "${DOCKERFILE_PATH}"
  args = {
    VERSION = "${VERSION}"
  }
}
