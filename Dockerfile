# Start from this base image, based on Ubuntu LTS.
FROM unitymultiplay/linux-base-image:1.0.1
ENV SERVER_BUILD_PATH=./Builds/ServerBuild/

# Install dependencies.
USER root
RUN apt-get update && apt-get install -y
USER mpukgame

# Set up a working directory.
WORKDIR /game

RUN ls -la /game

# Add your game files and perform any required init steps.
COPY ${SERVER_BUILD_PATH} .
# RUN ./init.sh

# Set your game entrypoint.
ENTRYPOINT [ "./ServerBuild.x86_64" ]
# CMD [ "--default", "args", "here" ]