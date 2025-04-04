from enum import Enum
import asyncio
from suikailauncher.core.base import exceptions,track
from suikailauncher.core.base.logger import logger
# Action 的状态
# Initialized 代表没有任务
# Runable 代表任务列表有任务，此时调用 start 方法会启动这个 Action
# Running 代表 Action 目前正在运行
# Complete 代表任务列表内的任务已经完成了
# Failed 代表任务列表内至少有一个任务失败了
# Timeout 代表执行超时
class ActionStatus(Enum):
    Initialized = 0
    Runable = 1
    Running = 2
    Complete = 3
    Failed = 4
    Timeout = 5


class TaskOutput:
    def __init__(self,task_id:int,**result):
        self.result = result
        self.task_id = task_id

class ActionOutput:
    def __init__(self,result:ActionStatus):
        self.result_code = result
        self.output = {}
    def output_result(self,task_output:TaskOutput):
        self.output[task_output.task_id] = task_output.result
class Action:
    def __init__(self,uuid:str,threads:int = 16):
        self.uuid = uuid
        self.tasks = []
        self.status = ActionStatus.Initialized
        self.task_list = asyncio.Queue(threads)
    def submit_task(self,ActionTask):
        if self.status == ActionStatus.Running:
            raise exceptions.InvalidOperationException("当前 Action 状态不允许添加任务")
        self.tasks.append(ActionTask)
        if self.status != ActionStatus.Runable:
            self.status = ActionStatus.Runable
    async def start(self,inputs):
        try:
            self.input = inputs
            if self.status != ActionStatus.Runable:
                return False
            self.status = ActionStatus.Running
            for task in self.tasks:
                await self.task_list.put(task)
            self.tasks = []
            results = await self.task_list.task_done()
            self.status = ActionStatus.Complete
            output = ActionOutput(self.status)
            for result in results:
                output.output_result(result)
            self.output = output
        except Exception as e:
            logger.error(f"[Action] 执行任务失败：{str(e)}\n详细信息:{track.get_ex_summary(e)}")