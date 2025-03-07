variable "IMAGE_NAME" {}
variable "VERSION" {}
variable "DOCKERFILE_PATH" {}

target "build" {
  inherits = ["base"]
  target = "build"
  tags = [
    "${IMAGE_NAME}:${VERSION}-builder",
  ]
  cache-from = [
      "type=gha,scope=build-${IMAGE_NAME}"
  ]
  cache-to = [
      "type=gha,mode=max,scope=build-${IMAGE_NAME}"
  ]
}

target "test" {
  inherits = ["base"]
  target = "test"
  tags = [
      "${IMAGE_NAME}:${VERSION}-builder",
  ]
  cache-from = [
      "type=gha,scope=test-${IMAGE_NAME}",
      "type=gha,scope=build-${IMAGE_NAME}"
  ]
  cache-to = [
      "type=gha,mode=max,scope=test-${IMAGE_NAME}"
  ]
}

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
