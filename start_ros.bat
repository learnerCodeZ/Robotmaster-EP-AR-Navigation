@echo off
echo ====================================
echo   ROS1 Noetic - Unity Bridge
echo ====================================
echo.

echo [1/3] Stopping old container (if running)...
docker stop ros_noetic >nul 2>&1

echo [1/3] Starting Docker container...
docker start ros_noetic
if %errorlevel% neq 0 (
    echo ERROR: Container 'ros_noetic' not found. Run:
    echo   docker run -it --name ros_noetic -v D:\Unity_Ros\ros1_ws:/ros1_ws -p 10000:10000 ros:noetic-ros-base
    pause
    exit /b 1
)

echo [2/3] Configuring ROS environment...
echo [3/3] Launching TCP endpoint on 0.0.0.0:10000...
echo.
echo ====================================
echo   Unity can now connect to:
echo   ROS IP: localhost
echo   ROS Port: 10000
echo ====================================
echo.

docker exec -it ros_noetic bash -c "source /opt/ros/noetic/setup.bash && cd /ros1_ws && source devel/setup.bash && roslaunch unity_bridge bridge.launch tcp_ip:=0.0.0.0 tcp_port:=10000"

pause
