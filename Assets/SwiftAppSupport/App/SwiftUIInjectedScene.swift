// Any swift file whose name ends in "InjectedScene" is expected to contain
// a computed static "scene" property like the one below. It will be injected to the top
// level App's Scene. The name of the class/struct must match the name of the file.

import Foundation
import SwiftUI


struct SwiftUIInjectedScene {
    @SceneBuilder
    static var scene: some Scene {
        WindowGroup(id: "ControlPanel") {
            ControlPanelHostView()
        }
        .defaultSize(width: 400.0, height: 400.0)
    }
}

private struct ControlPanelHostView: View {
    @StateObject private var model = AppModel()

    var body: some View {
        ControlPanelContentView()
            .environmentObject(model)
    }
}

// @Observable types can be used to store and update data that is presented in SwiftUI views
@Observable class ObjectCounter {
    var cubeCount: Int = 0
    var sphereCount: Int = 0
}
