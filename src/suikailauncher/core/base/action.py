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
    Pending = 6

class Task:
    def __init__(self,func):
        self.task_id = 0
        self.func = func
    async def run(self):
        error_track = ""
        try:
            result = await self.func
        except Exception as e:
            error_track = track.get_ex_summary(e)
        return TaskOutput(self.task_id,result if not error_track else error_track,ActionStatus.Complete if not error_track else ActionStatus.Failed)
    def assgin_id(self,task_id:int):
        self.task_id = task_id

class TaskOutput:
    def __init__(self,task_id:int,result,status:int):
        self.result = result
        self.task_id = task_id
        self.status = status

class ActionOutput:
    def __init__(self):
        self.data = {}
    def output_result(self,task_output:TaskOutput):
        self.data[str(task_output.task_id)] = task_output.result

class Action:
    def __init__(self,uuid:str,name:str = "未命名",max_coroutines:int = 64):
        self.uuid = uuid
        self.name = name
        self.status = ActionStatus.Initialized
        self.max_coroutines = max_coroutines
        self.task_list = []
        self.task_id_list = []
    async def submit_task(self,ActionTask:Task):
        if self.status == ActionStatus.Running:
            raise exceptions.InvalidOperationException("当前 Action 状态不允许添加任务")
        worker_id = len(self.task_list) + 1
        ActionTask.assgin_id(worker_id)
        self.task_id_list.append(str(worker_id))
        self.task_list.append(ActionTask.run())
        if self.status != ActionStatus.Runable:
            self.status = ActionStatus.Runable
    async def start(self):
        try:
            logger.info(f"[Action] {self.name}，状态改变：Running")
            result = []
            if self.status != ActionStatus.Runable:
                return False
            self.status = ActionStatus.Running
            result = await asyncio.gather(*self.task_list)
            output = ActionOutput()
            for _result in result:
                if _result.status == ActionStatus.Failed:
                    logger.error(f"[Action] 执行任务时发生错误\n任务输出：{_result.result}")
                output.output_result(_result)
            self.output = output
            self.status = ActionStatus.Complete
            logger.info(f"[Action] {self.name} 状态改变：Completed")
            return True

        except Exception as e:
            logger.error(f"[Action] {self.name} 状态改变：Failed，\n末输出：{track.get_ex_summary(e)}")
            self.status = ActionStatus.Failed
            return True
