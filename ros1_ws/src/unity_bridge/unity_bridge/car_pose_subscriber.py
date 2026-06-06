import rospy
from geometry_msgs.msg import PoseStamped


class CarPoseSubscriber:

    def __init__(self):
        rospy.Subscriber('/car_pose', PoseStamped, self.listener_callback)

    def listener_callback(self, msg):
        pos = msg.pose.position
        ori = msg.pose.orientation
        rospy.loginfo(
            f'Car pose: x={pos.x:.3f}, y={pos.y:.3f}, z={pos.z:.3f} | '
            f'qx={ori.x:.3f}, qy={ori.y:.3f}, qz={ori.z:.3f}, qw={ori.w:.3f}'
        )


def main():
    rospy.init_node('car_pose_subscriber')
    node = CarPoseSubscriber()
    rospy.spin()


if __name__ == '__main__':
    main()
