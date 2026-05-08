from odes import euler, euler_improved, runge_kutta_2, runge_kutta_4, picard

def solve_euler(expression: str, initial_x: float, initial_y: float, target_x: float, step_size: float) -> str:
    return euler.solve(expression, initial_x, initial_y, target_x, step_size)

def solve_euler_improved(expression: str, initial_x: float, initial_y: float, target_x: float, step_size: float) -> str:
    return euler_improved.solve(expression, initial_x, initial_y, target_x, step_size)

def solve_runge_kutta_2(expression: str, initial_x: float, initial_y: float, target_x: float, step_size: float) -> str:
    return runge_kutta_2.solve(expression, initial_x, initial_y, target_x, step_size)

def solve_runge_kutta_4(expression: str, initial_x: float, initial_y: float, target_x: float, step_size: float) -> str:
    return runge_kutta_4.solve(expression, initial_x, initial_y, target_x, step_size)

def solve_picard(expression: str, initial_x: float, initial_y: float, target_x: float, step_size: float, picard_order: int) -> str:
    return picard.solve(expression, initial_x, initial_y, target_x, step_size, picard_order)
